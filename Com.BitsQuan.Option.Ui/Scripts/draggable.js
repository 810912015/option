(function (window) {
    window.Draggable = function (handleElem, draggableElem) {
        var self = this;
        self.handle = handleElem;
        self.draggable = draggableElem;
        self.lastMousePosition = {x: 0, y: 0};

        self.handle.style['cursor'] = 'move';

        attachEvent(self.handle, 'mousedown', function (event) {
            event = event || window.event;
            self.isDragging = true;
            self.lastMousePosition.x = event.clientX;
            self.lastMousePosition.y = event.clientY;
        });
        attachEvent(self.handle, 'mouseup', attachEvent(window.document, 'mouseup', function () {
            self.isDragging = false;
        }));
        attachEvent(window.document.body, 'mousemove', function (event) {
            if (self.isDragging) {
                event = event || window.event;

                var offsetX = event.clientX - self.lastMousePosition.x,
                    offsetY = event.clientY - self.lastMousePosition.y;

                if (self.draggable.offsetLeft + offsetX >= 0
                    && self.draggable.offsetLeft + offsetX + self.draggable.scrollWidth <= window.document.documentElement.clientWidth) {
                    self.draggable.style["left"] = parseInt(self.draggable.style["left"]) + offsetX + "px";
                }

                if (self.draggable.offsetTop + offsetY >= 0
                    && self.draggable.offsetTop + offsetY + self.draggable.scrollHeight <= window.document.documentElement.clientHeight) {
                    self.draggable.style["top"] = parseInt(self.draggable.style["top"]) + offsetY + "px";
                }

                self.lastMousePosition.x = event.clientX;
                self.lastMousePosition.y = event.clientY;
            }
        });
    };

    function attachEvent(element, type, handler) {
        if (element.addEventListener) {
            element.addEventListener(type, handler, false);
        } else {
            element.attachEvent('on' + type, handler);
        }
        return handler;
    }
})(this);