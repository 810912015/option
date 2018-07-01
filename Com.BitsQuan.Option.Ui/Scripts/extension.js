function addAntiForgeryToken(obj) {
    var elms = document.getElementsByName("__RequestVerificationToken");
    if(elms.length){
        obj.__RequestVerificationToken = elms[0].value;
    }
    return obj;
};