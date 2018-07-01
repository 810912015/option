(function (global) {
    var zIndex = 8888;
    global.Popup = function (childElement, width /* String */, height /* String */, maskColor) {
        var self = this;
        self.popup = createPopup(childElement, width, height);
        document.body.appendChild(self.popup);

        self.show = function () {
            self.mask = self.mask || createMask(maskColor);
            bringToFront(self.mask);
            bringToFront(self.popup);
            document.body.appendChild(self.mask);
            self.popup.style.display = self.mask.style.display = 'block';
            center(self.popup);
            center(childElement);
        };
        self.close = function () {
            self.popup.style.display = 'none';
            self.mask.parentNode.removeChild(self.mask);
            self.mask = null;
            zIndex -= 2;
        };
    }
    
    /**
     * 把元素居中
     */
    function center(element) {
        element.style.left = (global.document.documentElement.clientWidth - element.scrollWidth) / 2 + 'px';
        element.style.top = (global.document.documentElement.clientHeight - element.scrollHeight) / 2 + 'px';
    }
    function bringToFront(element) {
        element.style.zIndex = zIndex;
        ++zIndex;
    }
    function setOpcity(element, percent) {
        element.style.opacity = percent.toString();
        element.style.filter = "alpha(opacity=" + (percent * 100) + ")";
        element.style["2-moz-opacity"] = percent.toString();
    }

    /**
     * 创建遮罩层
     */
    function createMask(maskColor) {
        maskColor = maskColor || '#000000';
        var mask = document.createElement("div");
        mask.id = "mask" + new Date().valueOf();
        mask.style.position = "fixed";
        mask.style.left = "0";
        mask.style.top = "0";
        mask.style.display = 'none';
        mask.style.width = document.documentElement.clientWidth + 'px';
        mask.style.height = document.documentElement.clientHeight + 'px';
        mask.style.backgroundColor = maskColor;
        setOpcity(mask, 0.3);
        return mask;
    }
    
    /**
     * 创建弹出层
     */
    function createPopup(childElement, width, height) {
        width = width || childElement.style.width;
        height = height || childElement.style.height;

        var popup = document.createElement("div");
        popup.id = "popup" + new Date().valueOf();
        popup.style.position = "fixed";
        popup.style.display = 'none';
        popup.style.width = width;
        popup.style.height = height;
        if (childElement.style.display == 'none')
            childElement.style.display = 'block';
        popup.appendChild(childElement);
        return popup;
    }
})(this);