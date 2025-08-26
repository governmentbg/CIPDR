// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    [Comment("Работни дни")]
    public class CalendarDay : EntityBaseWithLastModifiedInfo
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Column(TypeName = AttributeConstants.Date)]
        public DateTime CurrentDate { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string KindId { get; set; } = null!;
       
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string? Description { get; set; }

    }
}
