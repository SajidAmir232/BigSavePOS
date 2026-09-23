using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace POSApp.Data.Services;

public class EmailService
{
    private readonly string _gmailEmail;
    private readonly string _gmailPassword;
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;

    public EmailService(string gmailEmail, string gmailPassword)
    {
        _gmailEmail = gmailEmail;
        _gmailPassword = gmailPassword;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string resetLink)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("POS System", _gmailEmail));
            message.To.Add(new MailboxAddress("User", recipientEmail));
            message.Subject = "🔐 Password Reset Request - POS System";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .header {{ background-color: #3B82F6; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 20px; }}
        .button {{ background-color: #10B981; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 20px; text-align: center; }}
        .warning {{ color: #DC2626; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔐 Password Reset Request</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>We received a request to reset your password for your POS System account. Click the button below to proceed:</p>
            <a href='{resetLink}' class='button'>Reset Your Password</a>
            <p><strong>Or copy this link:</strong></p>
            <p>{resetLink}</p>
            <p class='warning'>⚠️ This link will expire in 24 hours.</p>
            <p>If you didn't request this, please ignore this email. Your password won't change until you confirm the reset.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 POS System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_gmailEmail, _gmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Email send error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string recipientEmail, string username)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("POS System", _gmailEmail));
            message.To.Add(new MailboxAddress(username, recipientEmail));
            message.Subject = "👋 Welcome to POS System!";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .header {{ background-color: #10B981; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 20px; }}
        .feature {{ margin: 15px 0; padding: 10px; background-color: #f0f0f0; border-left: 4px solid #3B82F6; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>👋 Welcome, {username}!</h2>
        </div>
        <div class='content'>
            <p>Your POS System account has been created successfully!</p>
            <h3>🎯 Get Started:</h3>
            <div class='feature'>
                <strong>📦 Inventory Management</strong><br>
                Add products, manage stock, track categories
            </div>
            <div class='feature'>
                <strong>🧾 POS Checkout</strong><br>
                Create sales invoices, manage cart, auto-reduce stock
            </div>
            <div class='feature'>
                <strong>🛒 Supplier Management</strong><br>
                Manage suppliers, create purchase invoices, track payments
            </div>
            <div class='feature'>
                <strong>🔄 Cloud Sync</strong><br>
                Automatic synchronization across multiple devices
            </div>
            <p><strong>Login Details:</strong></p>
            <p>Email: {recipientEmail}</p>
            <p>You can now login with your email and password!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 POS System. All rights reserved.</p>
            <p>If you have any questions, please contact support.</p>
        </div>
    </div>
</body>
</html>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_gmailEmail, _gmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Email send error: {ex.Message}");
            return false;
        }
    }
}
