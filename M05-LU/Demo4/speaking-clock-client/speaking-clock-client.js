const dotenv = require('dotenv');
const path = require('path')
var log = console.log;
const stream =  require('stream');
const date = require('date-and-time')
const dateFormat = require('dateformat');

const zeroPad = (num, places) => String(num).padStart(places, '0')

const Mic = require('node-microphone');
const sdk = require('microsoft-cognitiveservices-speech-sdk');
const Speaker = require('speaker');
const fs = require("fs");


function main() {

    try{
        // Get Configuration Settings
        const ENV_FILE = path.join(__dirname, '.env');
        dotenv.config({ path: ENV_FILE });
        lu_prediction_key = process.env.LU_PREDICTION_KEY;
        lu_prediction_region = process.env.LU_PREDICTION_REGION;
        lu_app_id = process.env.LU_APP_ID;

    // Configure speech service and get intent recognizer
    const mic = new Mic({rate: 8000, bitwidth : 8});
    const micStream = mic.startRecording();

    micStream.on('data', d => pushStream.write(d.slice()));
    micStream.on('end', () => {
        pushStream.close();
    });
            
    setTimeout(function () {
        mic.stopRecording(); // Stop recording after 5sec
    }, 5000)

    var pushStream = sdk.AudioInputStream.createPushStream()
    var audioConfig = sdk.AudioConfig.fromStreamInput(pushStream);
    speech_config = sdk.SpeechConfig.fromSubscription(lu_prediction_key, lu_prediction_region)
    speech_config.speechRecognitionLanguage = "en-US";
    var recognizer = new sdk.IntentRecognizer(speech_config, audioConfig);


    // Get the model from the AppID and add the intents we want to use
    var model = sdk.LanguageUnderstandingModel.fromAppId(lu_app_id);
    recognizer.addIntent(model, "GetTime");
    recognizer.addIntent(model, "GetDate");
    recognizer.addIntent(model, "GetDay");
    recognizer.addIntent(model, "None");
    recognizer.addAllIntents(model);

    log("Speak Now");

  // Process speech input
  recognizer.recognizeOnceAsync(
    function (result) {
        var intent = ''
        log("(continuation) Reason: " + sdk.ResultReason[result.reason]);
        switch (result.reason) {
            case sdk.ResultReason.RecognizedSpeech:
                // Speech was recognized, but no intent was identified.
                intent = result.text;
                log(`I don't know what ${intent} means.`);
                break;
            case sdk.ResultReason.RecognizedIntent:
                intent = result.intentId;
                log(`Query: ${result.text}`);
                log(`Intent: ${result.intentId}`);
                var jsonBody = result.properties.getProperty(sdk.PropertyId.LanguageUnderstandingServiceResponse_JsonResult)
                json = JSON.parse(jsonBody);
                log(`JSON Response:\n${jsonBody}\n`)
                var entity_value = ''
                var entity_type = ''

                // Get the first entity (if any)
                if (json["entities"].length > 0) {
                    entity_type = json["entities"][0]["type"]
                    entity_value = json["entities"][0]["entity"]
                    log(entity_type + ': ' + entity_value)
                }
                    
                    // Apply the appropriate action
                    if (intent == 'GetTime') {
                        var location = 'local';
                        // Check for entities
                        if (entity_type == 'Location')
                            location = entity_value;
                        // Get the time for the specified location
                        log(`taking time in ${location}`);
                        log(GetTime(location));
                    }
                    else if (intent == 'GetDay') {
                        var date_string = dateFormat(new Date(), "mm/dd/yyyy");
                        // Check for entities
                        if (entity_type == 'Date')
                            date_string = entity_value;
                            // Get the day for the specified date
                            log(GetDay(date_string));
                    }
                    else if (intent == 'GetDate') {
                            var day = 'today';
                            // Check for entities
                            if (entity_type == 'Weekday')
                                // List entities are lists
                                day = entity_value;
                            // Get the date for the specified day
                            log(GetDate(day));
                    }
                    else {
                            // Some other intent (for example, "None") was predicted
                            log(`You said ${result.text}`)
                            if (result.text.toLowerCase().replace('.', '') == 'stop')
                                intent = result.text;
                            else
                                log('Try asking me for the time, the day, or the date.')
                    }
               

                break;
            case sdk.ResultReason.NoMatch:
                // Speech wasn't recognized
                log("Sorry. I didn't understand that.");
                break;
            case sdk.ResultReason.Canceled:
                // Something went wrong
                var cancelDetails = sdk.CancellationDetails.fromResult(result);
                log(`Intent recognition canceled: ${sdk.CancellationReason[cancelDetails.reason]}`);
                if (cancelDetails.reason === sdk.CancellationReason.Error) {
                    log(cancelDetails.errorDetails);
                }
          break;
    }
}); 
 

    }
    catch (ex) {
        log(ex)
    }
}

function GetTime(location) {
    var time_string = ''

    /* Note: To keep things simple, we'll ignore daylight savings time and support only a few cities.
    In a real app, you'd likely use a web service API (or write  more complex code!)
    Hopefully this simplified example is enough to get the the idea that you
    use LU to determine the intent and entitites, then implement the appropriate logic
    */
    var now = new Date();
    if (location.toLowerCase() == 'local') {
        time_string = `${now.getHours()}:${zeroPad(now.getMinutes(),2)}`;
    }else if (location.toLowerCase() == 'london') {
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}`;
    }else if (location.toLowerCase() == 'sydney') {
        now = date.addHours(new Date(),11);
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}`;
    }else if (location.toLowerCase() == 'new york') {
        now = date.addHours(new Date(),-5);
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}`;
    }else if (location.toLowerCase() == 'nairobi') {
        now = date.addHours(new Date(),3);
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}` 
    }else if (location.toLowerCase() == 'tokyo') {
        now = date.addHours(new Date(),9);
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}` 
    }else if (location.toLowerCase() == 'delhi') {
        now = date.addHours(new Date(),5.5);
        time_string = `${now.getUTCHours()}:${zeroPad(now.getUTCMinutes(),2)}`
    } else {
        time_string = `I don't know what time it is in ${location}`
    }
    return time_string
}

function GetDate(day){
    var date_string = 'I can only determine dates for today or named days of the week.'

    var weekdays = [
        "monday",
        "tuesday",
        "wednesday",
        "thusday",
        "friday",
        "saturday",
        "sunday"
    ]

    var today = new Date()

    // To keep things simple, assume the named day is in the current week (Sunday to Saturday)
    var day = day.toLowerCase()
    if (day == 'today') {
        date_string = dateFormat(today, "mm/dd/yyyy");  //today.strftime("%m/%d/%Y")
    } else if (weekdays.includes(day)) {

        todayNum = today.getDay();
        weekDayNum = weekdays.indexOf(day);
        offset = weekDayNum - todayNum;
        date_string = dateFormat(date.addDays(today,offset+1), "mm/dd/yyyy"); 
    }
    return date_string
}

function GetDay(date_string){
    // Note: To keep things simple, dates must be entered in US format (MM/DD/YYYY)
    try{
        date_object = Date.parse(date_string);
        day_string =  dateFormat(date_object,"dddd");
    }catch (ex) {
        day_string = 'Enter a date in MM/DD/YYYY format.'
    }
    return day_string
}

main();