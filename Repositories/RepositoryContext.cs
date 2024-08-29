using Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RepositoryContext : IdentityDbContext<User>
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {
        }

        public DbSet<FileAttachment> FileAttachments { get; set; }  // İsimlendirme düzeltmesi
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<TypingStatus> TypingStatuses { get; set; }
        public DbSet<UserChatRoom> UserChatRooms { get; set; }
        public DbSet<PendingMessage> PendingMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserChatRoom: Many-to-Many relationship between User and ChatRoom
            modelBuilder.Entity<UserChatRoom>()
     .HasKey(uc => new { uc.UserId, uc.ChatRoomId }); // Composite Key

            modelBuilder.Entity<UserChatRoom>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChatRooms)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChatRoom>()
                .HasOne(uc => uc.ChatRoom)
                .WithMany(cr => cr.UserChatRooms)
                .HasForeignKey(uc => uc.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Yeni ReceiverId yapılandırması
            modelBuilder.Entity<UserChatRoom>()
                .HasOne(uc => uc.Receiver)
                .WithMany()
                .HasForeignKey(uc => uc.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message: One-to-Many relationship with ChatRoom and User
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatRoom)
                .WithMany(cr => cr.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)  // Geriye dönük ilişki
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrivateMessage: One-to-One relationship with User (Sender and Receiver)
            modelBuilder.Entity<PrivateMessage>()
                .HasOne(pm => pm.Sender)
                .WithMany(u => u.SentPrivateMessages)
                .HasForeignKey(pm => pm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrivateMessage>()
                .HasOne(pm => pm.Receiver)
                .WithMany(u => u.ReceivedPrivateMessages)
                .HasForeignKey(pm => pm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // FileAttachment: One-to-Many relationship with Message
            modelBuilder.Entity<FileAttachment>()
                .HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // TypingStatus: One-to-One relationship with User and ChatRoom
            modelBuilder.Entity<TypingStatus>()
                .HasOne(ts => ts.User)
                .WithMany(u => u.TypingStatuses)
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TypingStatus>()
                .HasOne(ts => ts.ChatRoom)
                .WithMany(cr => cr.TypingStatuses)
                .HasForeignKey(ts => ts.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PendingMessage>()
               .HasOne(pm => pm.Sender)
               .WithMany()
               .HasForeignKey(pm => pm.SenderId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingMessage>()
                .HasOne(pm => pm.Receiver)
                .WithMany()
                .HasForeignKey(pm => pm.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.ApplyConfiguration(new IdentityRoleConfig());
        }

    }


}

