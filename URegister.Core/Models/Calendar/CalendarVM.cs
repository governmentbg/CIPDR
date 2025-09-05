// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace URegister.Core.Models
{
    public class CalendarVM
    {
       
        public int Id { get; set; }

        [Display(Name = "Ден")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public DateTime CurrentDate { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Вид на деня")]
        public string KindId { get; set; } = null!;

        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string? Description { get; set; }
    }
}
