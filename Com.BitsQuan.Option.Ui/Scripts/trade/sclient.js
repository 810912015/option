 
       $(function () {
           var trade = $.connection.tradeHub;
           trade.client.Finish = function (o1) {
               var o = JSON.parse(o1);
               jsModel.updateOrder(o);
           };
           trade.client.PartialFinish = function (o2) {
               var o = JSON.parse(o2);

               jsModel.updateOrder(o);
           };
           trade.client.OrderRemoved = function(o){
               jsModel.removeFromRedo(o.Id);
           };
           trade.client.Position = function (upm) {
               // console.log(upm);
               jsModel.UpdatePosition(upm);
               jsModel.UpdateSta();
               ResetTip();
           };
           trade.client.Msg = function (m) {
               var dr = "<div>" + m + "</div>";
               $(dr).prependTo($("#srvtip"));
           };
           $.connection.hub.start();
       });
 