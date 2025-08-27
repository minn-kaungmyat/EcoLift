using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models.ViewModels
{
    public class InboxViewModel
    {
        public List<ConversationSummaryViewModel> Conversations { get; set; } = new List<ConversationSummaryViewModel>();
    }

    public class ConversationSummaryViewModel
    {
        public int ConversationId { get; set; }
        public int TripId { get; set; }
        public string TripRoute { get; set; } = string.Empty;
        public string OtherParticipantName { get; set; } = string.Empty;
        public string OtherParticipantId { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        
        // Computed properties
        public string LastMessagePreview => 
            LastMessage.Length > 50 ? LastMessage.Substring(0, 47) + "..." : LastMessage;
        
        public string TimeAgo => GetTimeAgo(LastMessageTime);
        
        private string GetTimeAgo(DateTime time)
        {
            var timeSpan = DateTime.UtcNow - time;
            
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            
            return "Just now";
        }
    }

    public class ChatViewModel
    {
        public int ConversationId { get; set; }
        public int TripId { get; set; }
        public string TripRoute { get; set; } = string.Empty;
        public string OtherParticipantName { get; set; } = string.Empty;
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        
        [Required(ErrorMessage = "Message cannot be empty")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string NewMessage { get; set; } = string.Empty;
    }

    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsFromCurrentUser { get; set; }
        public string SenderName { get; set; } = string.Empty;
        
        // Computed properties
        public string TimeDisplay => SentAt.ToString("HH:mm");
        public string DateDisplay => SentAt.ToString("MMM dd, yyyy");
        public bool ShowDateHeader { get; set; } = false;
    }

    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Message content is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;
        
        public int ConversationId { get; set; }
    }
}
