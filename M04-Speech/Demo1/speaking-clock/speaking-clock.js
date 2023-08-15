const dotenv = require('dotenv');
const path = require('path')
const stream =  require('stream');
const Speaker = require('speaker');
const log = console.log;

    // Import namespaces
    const sdk = require("microsoft-cognitiveservices-speech-sdk");
    const Mic = require('node-microphone');


var cog_key;
var cog_region;
var speech_config;


function TranscribeCommand() {
    
    return new Promise(resolve => {
            var command = ''
            
            // Configure speech recognition
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
            speech_config.speechRecognitionLanguage = "en-US";
            var recognizer = new sdk.SpeechRecognizer(speech_config, audioConfig);
            log('Speak now...')
            
            // Process speech input
            recognizer.recognizeOnceAsync(
                function (result) {
                    var document = JSON.parse(result.privJson);
                    if (document.RecognitionStatus=='Success') {
                        command = document.DisplayText;
                        log(`>${command}`);
                        resolve(command);                        
                    } else {
                        log(`RecognitionStatus: ${document.RecognitionStatus}`)
                        resolve('');
                    }
                    recognizer.close();
                    recognizer = undefined;
                    },
                    function (err) {
                        console.trace("err - " + err);
                        recognizer.close();
                        recognizer = undefined;
                    });          

    }); 
}

function TellTime() {

    var now = new Date()
    var minute = ('0' + now.getMinutes()).slice(-2);
    var response_text = `-The time is ${now.getHours()}:${minute}`;

    log(response_text);

    // Configure speech synthesis
    var speechConfig = sdk.SpeechConfig.fromSubscription(cog_key, cog_region);
    speechConfig.speechSynthesisVoiceName = "en-GB-George";
    var pstream = sdk.AudioOutputStream.createPullStream();
    var synaudioConfig = sdk.AudioConfig.fromStreamOutput(pstream);
    var synthesizer = new sdk.SpeechSynthesizer(speechConfig, synaudioConfig);
    var speaker = new Speaker({ channels: 1, sampleRate: 16000, bitDepth: 16 });
    
        // Synthesize spoken output
        response_text = `<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
        <voice name='en-US-JennyMultilingualNeural'>
            ${response_text}
            <break strength='weak'/>
            Time to have a brake!
            </voice>
        </speak>`;

        synthesizer.speakSsmlAsync(
            response_text,
            result => {
                if (result) {
                    synthesizer.close();
                    let bufferStream = new stream.PassThrough();
                    bufferStream.end(Buffer.from(result.audioData));
                    bufferStream.pipe(speaker);
                }
            },
            error => {
                console.log(error);
                synthesizer.close();
            }); 
}

async function main() {
    try {

        // Get Configuration Settings
        const ENV_FILE = path.join(__dirname, '.env');
        dotenv.config({ path: ENV_FILE });
        cog_key = process.env.COG_SERVICE_KEY;
        cog_region = process.env.COG_SERVICE_REGION;

        // Configure speech service
        speech_config = sdk.SpeechConfig.fromSubscription(cog_key, cog_region)
        log(`Ready to use speech service in:${speech_config.region}`)


        // Get spoken input
        var command = await TranscribeCommand() ;
        if (command != undefined && command.toLowerCase() == 'what time is it?') {
            TellTime();
        }
    }
    catch (ex) {
        log(ex)
    }

}
main();
