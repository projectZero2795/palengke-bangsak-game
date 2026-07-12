mergeInto(LibraryManager.library, {
  PalengkeGetAccessToken: function () {
    var key = "palengke_access_token";
    var token = "";
    try {
      token = window.localStorage.getItem(key) || "";
    } catch (_) {}
    if (!token) {
      var prefix = key + "=";
      var cookies = document.cookie ? document.cookie.split(";") : [];
      for (var i = 0; i < cookies.length; i += 1) {
        var cookie = cookies[i].trim();
        if (cookie.indexOf(prefix) === 0) {
          token = decodeURIComponent(cookie.substring(prefix.length));
          break;
        }
      }
    }
    var size = lengthBytesUTF8(token) + 1;
    var buffer = _malloc(size);
    stringToUTF8(token, buffer, size);
    return buffer;
  },

  PalengkeFreeString: function (pointer) {
    _free(pointer);
  },

  PalengkeRequestAccessToken: function (gameObjectNamePointer, callbackMethodPointer) {
    var gameObjectName = UTF8ToString(gameObjectNamePointer);
    var callbackMethod = UTF8ToString(callbackMethodPointer);
    var bridgeOrigin = "https://palengke.es";
    var requestType = "palengke-bang-sak-auth-request";
    var responseType = "palengke-bang-sak-auth-response";
    var readyType = "palengke-bang-sak-auth-ready";
    var randomBytes = new Uint8Array(16);

    if (!window.crypto || !window.crypto.getRandomValues) {
      SendMessage(gameObjectName, callbackMethod, "");
      return;
    }
    window.crypto.getRandomValues(randomBytes);
    var requestId = Array.prototype.map.call(randomBytes, function (value) {
      return value.toString(16).padStart(2, "0");
    }).join("");

    var iframe = document.createElement("iframe");
    iframe.src = bridgeOrigin + "/api/game-auth/bang-sak";
    iframe.title = "Palengke authentication bridge";
    iframe.setAttribute("aria-hidden", "true");
    iframe.setAttribute("sandbox", "allow-scripts allow-same-origin");
    iframe.style.display = "none";

    var finished = false;
    var timeoutId = 0;
    var cleanup = function () {
      window.removeEventListener("message", onMessage);
      if (timeoutId) window.clearTimeout(timeoutId);
      if (iframe.parentNode) iframe.parentNode.removeChild(iframe);
    };
    var finish = function (token) {
      if (finished) return;
      finished = true;
      cleanup();
      SendMessage(gameObjectName, callbackMethod, token || "");
    };
    var sendRequest = function () {
      if (!finished && iframe.contentWindow) {
        iframe.contentWindow.postMessage({ type: requestType, requestId: requestId }, bridgeOrigin);
      }
    };
    var onMessage = function (event) {
      if (event.origin !== bridgeOrigin || event.source !== iframe.contentWindow) return;
      var message = event.data;
      if (!message) return;
      if (message.type === readyType) {
        sendRequest();
        return;
      }
      if (message.type !== responseType || message.requestId !== requestId) return;
      finish(typeof message.accessToken === "string" ? message.accessToken : "");
    };

    window.addEventListener("message", onMessage);
    iframe.addEventListener("load", sendRequest, { once: true });
    timeoutId = window.setTimeout(function () { finish(""); }, 8000);
    document.body.appendChild(iframe);
  }
});
