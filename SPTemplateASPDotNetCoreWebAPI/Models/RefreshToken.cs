using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SPTemplateASPDotNetCoreWebAPI.Models
{
    [Owned]
    public class RefreshToken
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        //public string CreatedByIp { get; set; }
        //public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; } = string.Empty;
        
    }
}
     