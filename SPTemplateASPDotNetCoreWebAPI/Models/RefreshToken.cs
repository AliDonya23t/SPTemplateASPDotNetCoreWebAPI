/// <summary>
/// Description :
/// The refresh token entity class represents the data for a refresh token in the application.
/// [Owed] - meaning it can only exist as a child / dependant of another entity class. In this example a refresh token is always owned by a user entity.
///[JsonIgnore] - attribute prevents the id from being serialized and returned with refresh token data in api responses.

/// </summary>
///
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
        public string ReplacedByToken { get; set; } = string.Empty;
        
    }
}
     