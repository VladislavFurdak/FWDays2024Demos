var BrowserHelpers = {};
(function () {
    let ttimer;
    let chat_area;
    let caller;
    let scrollHeight_old;
    let scrollHeight_new;
    let height_load_zone_size = 25;
    let chatAreaId = '#chat-area';
    
    function throttle(callback, time) {
        if (ttimer) return;
        ttimer = true;

        setTimeout(() => {
            callback();
            ttimer = false;
        }, time);
    }

    function watchScrollZone(ev) {
        throttle(async ()  => {
            if (ev.target.scrollTop <= height_load_zone_size) {

                scrollHeight_old = chat_area.scrollHeight;
                await caller.invokeMethodAsync('LoadMoreMessages');
                scrollHeight_new = chat_area.scrollHeight;

                saveScrollPosition();
            }
        }, 500);
    }

    BrowserHelpers.Initialize = function (vCaller) {
        caller = vCaller;
    };

    function saveScrollPosition(){
        setTimeout(() => {
            chat_area.scrollTop = chat_area.scrollTop  + (scrollHeight_new - scrollHeight_old);
        }, 0)
    }

    BrowserHelpers.InitScrollWatch  = function () {
        chat_area = document.querySelector(chatAreaId);
        chat_area.addEventListener('scroll', watchScrollZone);
    }

    BrowserHelpers.HeightOfNewMessages = function(ids){
        let messages_height_sum = 0;
        let padding = 10;
        for (let i = 0; i < ids.length; i++) {
            messages_height_sum += document.getElementById(ids[i]).offsetHeight + padding;
        }
        return messages_height_sum;
    }
    
    BrowserHelpers.HeightOfChatArea = function(){
        return document.querySelector(chatAreaId).offsetHeight;
    }

    BrowserHelpers.ScrollToSeparator = function() {
        setTimeout(() => {
            document.getElementById("new-messages-separator").scrollIntoView();
        },0);
    }
    
})();