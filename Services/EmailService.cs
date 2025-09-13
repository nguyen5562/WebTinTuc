using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace WebTinTuc.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationToken, int userId)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Web Tin Tức";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(userName, toEmail));
                message.Subject = "Xác nhận tài khoản Tác giả - Web Tin Tức";

                // Tạo link xác nhận
                var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7104";
                var confirmationUrl = $"{baseUrl}/Account/ConfirmEmail?userId={userId}&token={confirmationToken}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = CreateEmailTemplate(userName, confirmationUrl);

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation("Email xác nhận đã được gửi đến {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email xác nhận đến {Email}", toEmail);
                return false;
            }
        }

        private string CreateEmailTemplate(string userName, string confirmationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận tài khoản Tác giả</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #007bff, #0056b3);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            background: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .btn {{
            display: inline-block;
            background: #28a745;
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .btn:hover {{
            background: #218838;
        }}
        .rules {{
            background: #e3f2fd;
            padding: 20px;
            border-left: 4px solid #2196f3;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            color: #666;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 Chào mừng bạn đến với Web Tin Tức!</h1>
        <p>Xác nhận tài khoản Tác giả của bạn</p>
    </div>
    
    <div class='content'>
        <h2>Xin chào {userName}!</h2>
        
        <p>Cảm ơn bạn đã đăng ký làm <strong>Tác giả</strong> tại Web Tin Tức. Để hoàn tất quá trình đăng ký và kích hoạt tài khoản, vui lòng nhấn vào nút bên dưới:</p>
        
        <div style='text-align: center;'>
            <a href='{confirmationUrl}' class='btn'>✅ Xác nhận Email & Kích hoạt tài khoản</a>
        </div>
        
        <div class='rules'>
            <h3>📋 Quy định dành cho Tác giả:</h3>
            <ul>
                <li>✅ Tuân thủ quy định viết bài và chịu trách nhiệm về nội dung bài viết</li>
                <li>✅ Không đăng nội dung vi phạm pháp luật hoặc không phù hợp</li>
                <li>✅ Tôn trọng quyền tác giả và bản quyền</li>
                <li>✅ Chịu trách nhiệm về tính chính xác của thông tin</li>
                <li>✅ Viết bài có chất lượng, hữu ích cho độc giả</li>
            </ul>
        </div>
        
        <p><strong>Lưu ý:</strong> Link xác nhận này sẽ hết hạn sau 24 giờ. Nếu bạn không thực hiện xác nhận trong thời gian này, vui lòng đăng ký lại.</p>
        
        <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
    </div>
    
    <div class='footer'>
        <p>© 2024 Web Tin Tức. Tất cả quyền được bảo lưu.</p>
        <p>Email này được gửi tự động, vui lòng không trả lời.</p>
    </div>
</body>
</html>";
        }
    }
}
