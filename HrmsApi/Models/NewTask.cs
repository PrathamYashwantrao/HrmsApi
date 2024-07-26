﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace HrmsApi.Models
{
  
    public class NewTask
    {
        public int ID { get; set; } // Primary key, generated by the database

        [Required]
        public string Batch { get; set; }

        [Required]
        public string Description { get; set; }

        public string AttachmentPath { get; set; }

        [Required]
        public DateTime CDate { get; set; }


     //   public string Students { get; set; }  // Comma-separated list of students


    }
   
    

}
