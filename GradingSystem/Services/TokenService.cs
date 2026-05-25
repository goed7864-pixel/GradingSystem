using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace GradingSystem.Services
{
    public class TokenService
    {
        private string _currentToken;

        public void SetToken(string token)
        {
            _currentToken = token;
        }

        public string GetCurrentUserRole()
        {
            if (string.IsNullOrEmpty(_currentToken))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(_currentToken);

                var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                    c.Type == "role");

                return roleClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public int? GetCurrentUserId()
        {
            if (string.IsNullOrEmpty(_currentToken))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(_currentToken);

                var idClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                    c.Type == "sub");

                if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
                    return userId;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public string GetCurrentUserEmail()
        {
            if (string.IsNullOrEmpty(_currentToken))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(_currentToken);

                var emailClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" ||
                    c.Type == "email");

                return emailClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public string GetCurrentUserName()
        {
            if (string.IsNullOrEmpty(_currentToken))
                return "Пользователь";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(_currentToken);

                // Пробуем найти claim с именем пользователя
                var nameClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" ||
                    c.Type == "name" ||
                    c.Type == "unique_name" ||
                    c.Type == "fullname" ||
                    c.Type == "FullName" ||
                    c.Type.EndsWith("/name"));

                // Если не нашли имя, пробуем использовать email
                if (nameClaim == null)
                {
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" ||
                        c.Type == "email");

                    if (emailClaim != null)
                    {
                        // Возвращаем часть email до @
                        var emailParts = emailClaim.Value.Split('@');
                        return emailParts.Length > 0 ? emailParts[0] : "Пользователь";
                    }
                }

                return nameClaim?.Value ?? "Пользователь";
            }
            catch
            {
                return "Пользователь";
            }
        }

        // Метод для отладки - показывает все claims в токене
        public string GetAllClaims()
        {
            if (string.IsNullOrEmpty(_currentToken))
                return "Токен отсутствует";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(_currentToken);

                return string.Join("\n", jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}"));
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
    }
}
