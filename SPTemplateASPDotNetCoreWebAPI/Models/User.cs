using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SPTemplateASPDotNetCoreWebAPI.Models
{
    /// <summary>
    /// Description :
    /// this is part of users table in witch register/login information is stored.
    /// Parameters :
    /// Id
    /// Username - user spcific name for login
    /// PasswordHash - user password hash with sha512 algorithem
    /// PasswordSalt - password salt added for each user so no 2 password hashs would be the same in database
    ///     for 2 users in case of same passwords 
    /// RefreshToken - the previously created refresh token for user 
    /// TokenCreated - time of token creation
    /// TokenExpires - time of token expiration
    /// </summary>
    ///
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
