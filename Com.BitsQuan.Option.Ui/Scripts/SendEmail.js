
    $(function () {
        $("#loginBtn").click(function () {
            var uurl = $("#email").html();
            alert("uurl:" + uurl);
            uurl = gotoEmail(uurl);
            if (uurl != "") {
                window.open("http://" + uurl);
            } else {
                alert("抱歉!未找到对应的邮箱登录地址，请自己登录邮箱查看邮件！");
            }
        });
        var Request = new Object();
        Request = GetRequest();
        $('#email').text((Request['email']));
    })



function GetRequest() {

    var url = location.search; //获取url中"?"符后的字串

    var theRequest = new Object();

    if (url.indexOf("?") != -1) {

        var str = url.substr(1);

        strs = str.split("&");

        for (var i = 0; i < strs.length; i++) {

            theRequest[strs[i].split("=")[0]] = unescape(strs[i].split("=")[1]);

        }

    }

    return theRequest;

}
//功能：根据用户输入的Email跳转到相应的电子邮箱首页
function gotoEmail(email) {
    //  var $t = $mail.split('@@')[1];
    var aa = email.split('@@')[1];
    alert("aa:"+aa);
    alert(aa.toString() == "163.com");
    //   alert("nlnl3:"+nn.equals("163.com"));
    //  $t = $t.toLowerCase();;
    //if ($t == "163.com") {
    //    alert("..");
    //    return 'mail.163.com';
    //} else if ($t == 'vip.163.com') {
    //    return 'vip.163.com';
    //} else if ($t == '126.com') {
    //    return 'mail.126.com';
    //} else if ($t == 'qq.com' || $t == 'vip.qq.com' || $t == 'foxmail.com') {
    //    return 'mail.qq.com';
    //} else if ($t == 'gmail.com') {
    //    return 'mail.google.com';
    //} else if ($t == 'sohu.com') {
    //    return 'mail.sohu.com';
    //} else if ($t == 'tom.com') {
    //    return 'mail.tom.com';
    //} else if ($t == 'vip.sina.com') {
    //    return 'vip.sina.com';
    //} else if ($t == 'sina.com.cn' || $t == 'sina.com') {
    //    return 'mail.sina.com.cn';
    //} else if ($t == 'tom.com') {
    //    return 'mail.tom.com';
    //} else if ($t == 'yahoo.com.cn' || $t == 'yahoo.cn') {
    //    return 'mail.cn.yahoo.com';
    //} else if ($t == 'tom.com') {
    //    return 'mail.tom.com';
    //} else if ($t == 'yeah.net') {
    //    return 'www.yeah.net';
    //} else if ($t == '21cn.com') {
    //    return 'mail.21cn.com';
    //} else if ($t == 'hotmail.com') {
    //    return 'www.hotmail.com';
    //} else if ($t == 'sogou.com') {
    //    return 'mail.sogou.com';
    //} else if ($t == '188.com') {
    //    return 'www.188.com';
    //} else if ($t == '139.com') {
    //    return 'mail.10086.cn';
    //} else if ($t == '189.cn') {
    //    return 'webmail15.189.cn/webmail';
    //} else if ($t == 'wo.com.cn') {
    //    return 'mail.wo.com.cn/smsmail';
    //} else if ($t == '139.com') {
    //    return 'mail.10086.cn';
    //} else {
    //    return '';
    //}
    return '';
};
