mergeInto(LibraryManager.library, {

  EnableHelpshift: function (guid) {
    var PLATFORM_ID = "gamingdemo_platform_20190415170138422-dfe89ece2efffd9",
        DOMAIN = "gamingdemo",
        LANGUAGE = "en";

    window.helpshiftConfig = {
        platformId: PLATFORM_ID,
        domain: DOMAIN,
        language: LANGUAGE,
        userId: "123456",
        userEmail: "Backtrace@Backtrace.io",
        userName: "John Doe"
    };

    !function(t,e){if("function"!=typeof window.Helpshift){var n=function(){n.q.push(arguments)};n.q=[],window.Helpshift=n;var i,a=t.getElementsByTagName("script")[0];if(t.getElementById(e))return;i=t.createElement("script"),i.async=!0,i.id=e,i.src="https://webchat.helpshift.com/webChat.js";var o=function(){window.Helpshift("init")};window.attachEvent?i.attachEvent("onload",o):i.addEventListener("load",o,!1),a.parentNode.insertBefore(i,a)}else window.Helpshift("update")}(document,"hs-chat");

    Helpshift("setCustomIssueFields", {
        // Key of the Custom Issue Field
        "segment": {
            // Type of Custom Issue Field
            type: "dropdown",
            // Value to set for Custom Issue Field
            value: "vip"
        },
        // Key of the Custom Issue Field
        "device_id": {
            // Type of Custom Issue Field
            type: "singleline",
            // Value to set for Custom Issue Field
            value: guid
        }
    });
  }

});