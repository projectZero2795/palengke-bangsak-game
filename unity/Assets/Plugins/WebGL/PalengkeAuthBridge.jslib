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
  }
});
