window.renderPaypalButtons = async function (config, planSelected, dotnetRef, containerId) {
    // Armar URL del SDK
    const query = new URLSearchParams({
        "client-id": config.clientId,
        "components": "buttons",
        "intent": "subscription",
        "vault": "true"
    }).toString();

    const sdkUrl = `https://www.paypal.com/sdk/js?${query}`;

    // Cargar SDK si no está cargado aún (o si cambió el client-id)
    const existing = document.querySelector(`script[data-pp-sdk][src^="https://www.paypal.com/sdk/js"]`);
    if (!existing) {
        await new Promise((resolve, reject) => {
            const s = document.createElement('script');
            s.src = sdkUrl;
            s.setAttribute('data-pp-sdk', '1');
            s.onload = resolve;
            s.onerror = reject;
            document.body.appendChild(s);
        });
    }

    // Elegir el Plan ID
    const planId =
        planSelected === 'Premium' ? config.premiumPlanId :
            (planSelected === 'Standard' ? config.standardPlanId : config.basicPlanId);

    const container = document.getElementById(containerId);
    if (!container) return;

    // Limpiar contenedor (si el usuario cambió de plan)
    container.innerHTML = "";

    // Render del botón
    paypal.Buttons({
        style: { layout: 'vertical', shape: 'rect' },
        createSubscription: function (data, actions) {
            return actions.subscription.create({ plan_id: planId });
        },
        onApprove: function (data, actions) {
            if (data.subscriptionID) {
                dotnetRef.invokeMethodAsync('OnPaypalApproved', data.subscriptionID);
            }
        },
        onError: function (err) {
            console.error('PayPal error', err);
            alert('Error con PayPal: ' + (err?.message || err));
        }
    }).render('#' + containerId);
};
