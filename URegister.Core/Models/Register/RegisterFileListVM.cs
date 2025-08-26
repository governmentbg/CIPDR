using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Register
{
    public class RegisterFileListVM
    {
        public string? FilesLabel { get; set; } = "Прикачени файлове";
        public List<RegisterFileVM> Files { get; set; } = new();
    }
}
