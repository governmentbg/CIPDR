// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models
{
    /// <summary>
    /// Модел за филтриране на данни за таботни/неработни дни
    /// </summary>
    public class CalendarFilterVM
    {
        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "Дата от")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "Дата до")]
        public DateTime DateTo { get; set; }
    }
}
