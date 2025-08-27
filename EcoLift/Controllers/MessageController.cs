using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoLift.Data;
using EcoLift.Models;
using EcoLift.Models.ViewModels;
using System.Security.Claims;

namespace EcoLift.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Message/Inbox
        public async Task<IActionResult> Inbox()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }
            
            // Get all conversations where the current user is either driver or passenger
            var conversations = await _context.Conversations
                .Include(c => c.Trip)
                .Include(c => c.Driver)
                .Include(c => c.Passenger)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1)) // Get last message
                .Where(c => c.DriverId == currentUserId || c.PassengerId == currentUserId)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            var inboxViewModel = new InboxViewModel
            {
                Conversations = conversations.Select(c => new ConversationSummaryViewModel
                {
                    ConversationId = c.ConversationId,
                    TripId = c.TripId,
                    TripRoute = c.TripRoute,
                    OtherParticipantName = c.OtherParticipantName(currentUserId),
                    OtherParticipantId = currentUserId == c.DriverId ? c.PassengerId : c.DriverId,
                    LastMessage = c.Messages.FirstOrDefault()?.Content ?? "No messages yet",
                    LastMessageTime = c.Messages.FirstOrDefault()?.SentAt ?? c.CreatedAt,
                    UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId)
                }).ToList()
            };

            return View(inboxViewModel);
        }

        // GET: Message/Chat/{conversationId}
        public async Task<IActionResult> Chat(int conversationId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var conversation = await _context.Conversations
                .Include(c => c.Trip)
                .Include(c => c.Driver)
                .Include(c => c.Passenger)
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
            {
                return NotFound();
            }

            // Check if current user is part of this conversation
            if (conversation.DriverId != currentUserId && conversation.PassengerId != currentUserId)
            {
                return Forbid();
            }

            // Mark messages as read
            var unreadMessages = conversation.Messages
                .Where(m => !m.IsRead && m.SenderId != currentUserId);
            
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            var chatViewModel = new ChatViewModel
            {
                ConversationId = conversation.ConversationId,
                TripId = conversation.TripId,
                TripRoute = conversation.TripRoute,
                OtherParticipantName = conversation.OtherParticipantName(currentUserId),
                Messages = conversation.Messages.Select(m => new MessageViewModel
                {
                    MessageId = m.MessageId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsFromCurrentUser = m.SenderId == currentUserId,
                    SenderName = m.SenderDisplayName
                }).ToList()
            };

            return View(chatViewModel);
        }

        // POST: Message/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Message content cannot be empty");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
            {
                return NotFound();
            }

            // Check if current user is part of this conversation
            if (conversation.DriverId != currentUserId && conversation.PassengerId != currentUserId)
            {
                return Forbid();
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
                };

                _context.Messages.Add(message);
            
            // Update conversation's last message time
            conversation.LastMessageAt = DateTime.UtcNow;
            
                await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { conversationId });
        }

        // POST: Message/CreateConversation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConversation(int tripId, string passengerId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
            {
                return NotFound();
            }

            // Check if current user is the driver of this trip
            if (trip.ProviderId != currentUserId)
            {
                return Forbid();
            }

            // Check if conversation already exists
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.TripId == tripId && 
                                        c.DriverId == currentUserId && 
                                        c.PassengerId == passengerId);

            if (existingConversation != null)
            {
                return RedirectToAction(nameof(Chat), new { conversationId = existingConversation.ConversationId });
            }

            var conversation = new Conversation
            {
                TripId = tripId,
                DriverId = currentUserId,
                PassengerId = passengerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { conversationId = conversation.ConversationId });
        }
    }
}
