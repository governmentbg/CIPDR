using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Core.Services
{

    public class PublicFieldTemplateService : BaseService, IPublicFieldTemplateService
    {
        private readonly ObjectsCatalogGrpcClient serviceGrpcClient;
        private readonly INomenclatureClientService nomenclatureClientService;
        public PublicFieldTemplateService(
          IApplicationRepository repo,
          ILogger<PublicFieldTemplateService> logger,
          ObjectsCatalogGrpcClient serviceGrpcClient,
          INomenclatureClientService nomenclatureClientService
       ) : base(repo, logger)
        {
            this.serviceGrpcClient = serviceGrpcClient;
            this.nomenclatureClientService = nomenclatureClientService;
        }
        public async Task<PublicFieldTemplateVM> GetTemplate(int id)
        {
            return await Repo.AllReadonly<PublicFieldTemplate>()
                          .Where(x => x.Id == id)
                          .Select(x => new PublicFieldTemplateVM
                          {
                              Id = x.Id,
                              FieldName = x.FieldName,
                              Label = x.Label,
                              Content = x.Content,
                          })
                          .TagWith(nameof(GetTemplate))
                          .FirstAsync();
        }

        public async Task AppendUpdate(PublicFieldTemplateVM model)
        {
            var data = new PublicFieldTemplate();
            if (model.Id > 0)
            {
                data = await Repo.All<PublicFieldTemplate>()
                                 .Where(x => x.Id == model.Id)
                                 .FirstAsync();
            }
            else
            {
                await Repo.AddAsync(data);
            }
            data.Label = model.Label;
            data.OrderNum = model.OrderNum;
            data.FieldName = model.FieldName;
            if (data.OrderNum <= 0)
            {
                data.OrderNum = (await Repo.AllReadonly<PublicFieldTemplate>()
                                           .MaxAsync(x => (int?)x.OrderNum)) ?? 0 ;
                data.OrderNum++;
            }
            data.IsActive = true;
            await Repo.SaveChangesAsync();
        }

        public async Task AppendUpdateContent(PublicFieldTemplateVM model)
        {
            var data = await Repo.All<PublicFieldTemplate>()
                                 .Where(x => x.Id == model.Id)
                                 .FirstAsync();
            data.Content = model.Content;
            await Repo.SaveChangesAsync();
        }

        public async Task<IActionResult> GetTemplateList(IDataTablesRequest request)
        {
            var data = Repo.AllReadonly<PublicFieldTemplate>()
                          .IgnoreQueryFilters()
                          .Where(x => x.IsActive)
                          .Select(x => new PublicFieldTemplateVM
                          {
                              Id = x.Id,
                              FieldName = x.FieldName,
                              Label = x.Label,
                              OrderNum = x.OrderNum,
                          })
                          .OrderBy(x => x.OrderNum)
                          .TagWith(nameof(GetTemplateList));
            var countAll = await data.CountAsync();
            var list = data.ToList();
            return request.GetResponseJson(data, countAll);
        }

        /// <summary>
        /// Изтрива бланка по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteTemplate(int id)
        {
            try
            {
                var template = await Repo.All<PublicFieldTemplate>()
                    .TagWith(nameof(DeleteTemplate))
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(f => f.Id == id);

                if (template == null)
                {
                    return new OperationResult($"Активна бланка с идентификатор {id} не е открита");
                }
                template.IsActive = false;
                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на формата с id {id}");
                return new OperationResult("Проблем при изтриване на формата");
            }
        }

        public async Task<List<BlanksTemplateParamVM>> GetTemplateParam(FormViewModel formModel, string prefix)
        {
            var paramListResult = new List<BlanksTemplateParamVM>();
            var paramList = formModel.FormFields
            .Where(x => x.IsPublic)
            .Select(x => new BlanksTemplateParamVM
            {
                Label = x.Label,
                Name = $"{prefix}{x.Name}",
                Type = x.Type,
                Repeatable = x.CanBeRepeated,
                Templates = x.Fields?.Select(f => new BlanksTemplateParamVM
                {
                    Label = f.Label,
                    Name = $"{prefix}{f.Name}",
                    Type = f.Type,
                    Repeatable = x.CanBeRepeated,
                }).ToList()
            })
           .ToList();
            var response = await serviceGrpcClient.GetFieldTemplateListAsync(new FieldTemplateListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 }
            });
            foreach (var param in paramList)
            {
                if (param.Repeatable)
                {
                    paramListResult.AddRange(response.FieldTemplates
                                             .Where(x => x.FieldType == param.Type)
                                             .Select(x => new BlanksTemplateParamVM
                                             {
                                                 Name = $"{prefix}{param.Name}_FieldTemplate{x.Id}",
                                                 Label = $"{param.Name} {x.Name}",
                                                 Repeatable = param.Repeatable,
                                             })
                                             .ToList());
                    continue;
                }
                paramListResult.Add(param);
                if (param.Templates?.Any() != true){
                    var newParam = new BlanksTemplateParamVM
                    {
                        Label = $"{param.Label}+",
                        Name = $"{param.Name}_WithPrefix",
                        Type = param.Type
                    };
                    paramListResult.Add(newParam); 
                }
                if (param.Templates?.Any() == true)
                {
                    var templates = new List<BlanksTemplateParamVM>();
                    foreach (var inner in param.Templates)
                    {
                        templates.Add(inner);
                        var newInner = new BlanksTemplateParamVM
                        {
                            Label = $"{inner.Label}+",
                            Name = $"{inner.Name}_WithPrefix",
                            Type = inner.Type
                        };
                        templates.Add(newInner);
                    }
                    param.Templates = templates;
                }
            }

            foreach (var param in paramListResult.Where(x => !x.Repeatable).ToList())
            {
                if (param.Templates?.Any() == true)
                {
                    param.Templates.AddRange(response.FieldTemplates
                                              .Where(x => x.FieldType == param.Type)
                                              .Select(x => new BlanksTemplateParamVM
                                              {
                                                  Name = $"{prefix}{param.Name}_FieldTemplate{x.Id}",
                                                  Label = x.Name,
                                              })
                                              .ToList());
                }
                if (param.Templates?.Any() != true)
                {
                    param.Templates = null;
                }
            }

            return paramListResult;
        }

        public async Task OrderNumChange(int id, bool up)
        {
            var data = await Repo.All<PublicFieldTemplate>()
                                     .IgnoreQueryFilters()
                                     .Where(x => x.IsActive)
                                     .OrderBy(x => x.OrderNum)
                                     .ToListAsync();
            for (int i = 0; i < data.Count;i++) {
                var item = data[i];
                PublicFieldTemplate itemChange = null;
                if (item.Id == id)
                {
                    if (up && i > 0)
                    {
                        itemChange = data[i - 1];
                    }
                    if (!up && i < (data.Count - 1))
                    {
                        itemChange = data[i + 1];
                    }
                    if (itemChange != null)
                    {
                        var orderNum = item.OrderNum;
                        item.OrderNum = itemChange.OrderNum;
                        itemChange.OrderNum = orderNum;
                        await Repo.SaveChangesAsync();
                    }

                }
            }
        }
        public async Task<List<PublicFieldTemplate>> GetTemplates()
        {
            return await Repo.AllReadonly<PublicFieldTemplate>()
                          .Where(x => x.IsActive)
                          .TagWith(nameof(GetTemplates))
                          .OrderBy(x => x.OrderNum)
                          .ToListAsync();
        }
    }
}
