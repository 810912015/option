using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Ui.Hubs
{
    public class SpotHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        static IHubContext _context;
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;

            _connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
        public override System.Threading.Tasks.Task OnConnected()
        {
            string name = Context.User.Identity.Name;


            _connections.Add(name, Context.ConnectionId);


            return base.OnConnected();
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
        public static void Init()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<SpotHub>();
            MvcApplication.SpotService.match.OnFinish += match_OnFinish;
            MvcApplication.SpotService.match.OnPartialFinish += match_OnPartialFinish;
        }

        static void match_OnPartialFinish(Core.Spot.SpotOrder obj)
        {
            var dto = new SpotOrderDto(obj);
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            foreach (var connectionId in _connections.GetConnections(obj.Trader.Name))
            {
                _context.Clients.Client(connectionId).SpotPartialFinish(str);
            }
        }

        static void match_OnFinish(Core.Spot.SpotOrder obj)
        {
            var dto = new SpotOrderDto(obj);
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            foreach (var connectionId in _connections.GetConnections(obj.Trader.Name))
            {
                _context.Clients.Client(connectionId).SpotFinish(str);
            }
        }
    }
    public class TradeHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        static IHubContext _context;

        public static void Init()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<TradeHub>();
            MvcApplication.OptionService.Matcher.OnFinish += Matcher_OnFinish;
            MvcApplication.OptionService.Matcher.OnPartialFinish += Matcher_OnPartialFinish;
            MvcApplication.OptionService.PosSvc.OnPositionSummaryChanged += OptionService_OnPositionSummaryChanged;
            MvcApplication.OptionService.OnUserMsge += Send;
            TraderService.OnBailPayWhenInsufficent += TraderService_OnBailPayWhenInsufficent;
        }

        public static void TraderOrderRemoved(Order order)
        {
            foreach (var connectionId in _connections.GetConnections(order.Trader.Name))
            {
                _context.Clients.Client(connectionId).OrderRemoved(order);
            }
        }
        static void TraderService_OnBailPayWhenInsufficent(Core.Trader arg1, decimal arg2)
        {
            Send(arg1.Name, string.Format("将要付款{0}时保证金不足", arg2));
        }
        static void Send(string name, string msg)
        {
            foreach (var v in _connections.GetConnections(name))
            {
                _context.Clients.Client(v).Msg(msg);
            }
        }
        static void OptionService_OnPositionSummaryChanged(string arg1, PositionSummaryDto arg2)
        {

            foreach (var v in _connections.GetConnections(arg1))
            {
                _context.Clients.Client(v).Position(arg2);
            }
        }


        static void Matcher_OnPartialFinish(Order arg1, int arg2)
        {
            var r = Newtonsoft.Json.JsonConvert.SerializeObject(new OrderDto(arg1));
            foreach (var connectionId in _connections.GetConnections(arg1.Trader.Name))
            {
                _context.Clients.Client(connectionId).PartialFinish(r);
            }
        }

        static void Matcher_OnFinish(Order obj)
        {
            var r = Newtonsoft.Json.JsonConvert.SerializeObject(new OrderDto(obj));
            foreach (var c in _connections.GetConnections(obj.Trader.Name))
            {
                _context.Clients.Client(c).Finish(r);
            }
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;

            _connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
        public override System.Threading.Tasks.Task OnConnected()
        {
            string name = Context.User.Identity.Name;


            _connections.Add(name, Context.ConnectionId);


            return base.OnConnected();
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }

    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}