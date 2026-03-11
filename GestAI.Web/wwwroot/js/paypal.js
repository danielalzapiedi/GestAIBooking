// (Optional) Externalized helpers if you prefer to reference from index.html
window.initPaypalSdk = async function(clientId) {
  const query = new URLSearchParams({
    "client-id": clientId,
    "components": "buttons",
    "intent": "subscription",
    "vault": "true"
  }).toString();
  const sdkUrl = `https://www.paypal.com/sdk/js?${query}`;
  if (!document.querySelector(`script[src^="https://www.paypal.com/sdk/js"]`)) {
    await new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = sdkUrl; s.onload = resolve; s.onerror = reject;
      document.body.appendChild(s);
    });
  }
};
