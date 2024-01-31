using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgReader.Mime;
using MsgReader.Mime.Header;

namespace GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv
{
    public class ExtractedEmailingEvent
    {
        public string MessageId { get; set; }
        public string Subject { get; set; }
        public string SenderEmail { get; set; }
        public string SenderDisplayName { get; set; }
        public string SendingMethod { get; set; }
        public DateTime DateSent { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverDisplayName { get; set; }
        public string TextBody { get; set; }
        public string AttachmentsRelativePath { get; set; }

        public static List<ExtractedEmailingEvent> GetExtractedEmailingEventsFromMimeMessage(Message message, string attachmentsPath)
        {
            string textBody;
            try
            {
                textBody = Encoding.UTF8.GetString(Encoding.Convert(message.TextBody.BodyEncoding, Encoding.UTF8, message.TextBody.Body));
            }
            catch
            {
                textBody = string.Empty;
            }

            List<ExtractedEmailingEvent> result = new List<ExtractedEmailingEvent>(message.Headers.Cc.Count + message.Headers.Bcc.Count + message.Headers.To.Count);
            foreach (var cc in message.Headers.Cc.Where(cc => cc != null))
            {
                ExtractedEmailingEvent newEvent = PrepareNewExtractedEmailingEvent(message.Headers, cc, "CC", textBody, attachmentsPath);
                result.Add(newEvent);
            }
            foreach (var bcc in message.Headers.Bcc.Where(bcc => bcc != null))
            {
                ExtractedEmailingEvent newEvent = PrepareNewExtractedEmailingEvent(message.Headers, bcc, "BCC", textBody, attachmentsPath);
                result.Add(newEvent);
            }
            foreach (var to in message.Headers.To.Where(to => to != null))
            {
                ExtractedEmailingEvent newEvent = PrepareNewExtractedEmailingEvent(message.Headers, to, "TO", textBody, attachmentsPath);
                result.Add(newEvent);
            }
            return result;
        }

        private static ExtractedEmailingEvent PrepareNewExtractedEmailingEvent
            (MessageHeader headers, RfcMailAddress receiver, string sendingMethod, string textBody, string attachmentsPath)
        {
            return new ExtractedEmailingEvent()
            {
                MessageId = GetSafeString(headers.MessageId),
                Subject = GetSafeString(headers.Subject),
                SenderEmail = GetSafeString(headers.From.Address),
                SenderDisplayName = GetSafeString(headers.From.DisplayName),
                SendingMethod = sendingMethod,
                DateSent = GetSafeDateTime(headers.DateSent),
                ReceiverEmail = GetSafeString(receiver.Address),
                ReceiverDisplayName = GetSafeString(receiver.DisplayName),
                TextBody = GetSafeString(textBody),
                AttachmentsRelativePath = GetSafeString(attachmentsPath)
            };
        }

        private static DateTime GetSafeDateTime(DateTime dateTime)
        {
            return (dateTime != null) ? dateTime : DateTime.MinValue;
        }

        private static string GetSafeString(string str)
        {
            return (str != null) ? str : string.Empty;
        }
    }
}
