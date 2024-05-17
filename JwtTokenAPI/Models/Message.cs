using MimeKit;

namespace JwtTokenAPI.Models
{
    public class Message
    {

        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<string> to, string subjet, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("Email", x)));
            Subject = subjet;
            Content = content;
        }
    }
}
