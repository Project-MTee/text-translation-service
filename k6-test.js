import http from "k6/http";

export default function () {
  var url = "http://localhost:5014/Text";
  var payload = JSON.stringify({
    srcLang: "en",
    trgLang: "et",
    domain: "general",
    text: ["translate me"],
  });

  var params = {
    headers: {
      "Content-Type": "application/json",
    },
  };

  http.post(url, payload, params);
}
