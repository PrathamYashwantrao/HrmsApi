﻿using System.ComponentModel.DataAnnotations;

namespace HrmsApi.Model
{
    public class f16
    {
        [Key]
        public int id {  get; set; }

        public string email { get; set; }

        public string attachment { get; set; }
    }
}
