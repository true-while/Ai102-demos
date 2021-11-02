
REM translate to Italian
curl -X POST "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=en&to=it" -H "Ocp-Apim-Subscription-Key: " -H "Content-Type: application/json; charset=UTF-8" -d "[{'Text':'Hello, what is your name?'}]"

REM translate to Spanish
curl -X POST "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=en&to=es" -H "Ocp-Apim-Subscription-Key: " -H "Content-Type: application/json; charset=UTF-8" -d "[{'Text':'Hello, what is your name?'}]"

REM Translate to German
curl -X POST "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=en&to=de" -H "Ocp-Apim-Subscription-Key: " -H "Content-Type: application/json; charset=UTF-8" -d "[{'Text':'Hello, what is your name?'}]"

@ECHO OFF
set /p key=""