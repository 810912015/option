using System;
namespace Com.BitsQuan.Miscellaneous
{
    public interface IMailSender
    {
        void SendTo(string to, string subject, string content);
        System.Threading.Tasks.Task SendToWait(string to, string subject, string content);
        
    }
    public interface IMsgSender
    {
        void SendMassage2(string phone, string str);
    }
}
