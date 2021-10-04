import http from "k6/http";

export default function () {
  var url = "https://localhost:50013/Text";
  var payload = JSON.stringify({
    srcLang: "en",
    trgLang: "lv",
    domain: "finance",
    text: ["translate me"],
  });

  var params = {
    headers: {
      "Content-Type": "application/json",
    },
  };

  http.post(url, payload, params);
}
