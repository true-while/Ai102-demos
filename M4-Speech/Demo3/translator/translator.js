const dotenv = require('dotenv');
const path = require('path')
var readline = require('readline');
var log = console.log;
const fs = require("fs");
const stream =  require('stream');

//Import namespaces
const Mic = require('node-microphone');
const sdk = require('microsoft-cognitiveservices-speech-sdk');
const Speaker = require('speaker');

var cog_key;
var cog_region;
var sTranslationConfig;

function main() {
    try { 
        // Get Configuration Settings
        const ENV_FILE = path.join(__dirname, '.env');
        dotenv.config({ path: ENV_FILE });
        cog_key = process.env.COG_SERVICE_KEY;
        cog_region = process.env.COG_SERVICE_REGION;

        // Configure translation
        sTranslationConfig = sdk.SpeechTranslationConfig.fromSubscription(cog_key, cog_region);
            
        sTranslationConfig.speechRecognitionLanguage = "en-US";
        sTranslationConfig.addTargetLanguage("fr");
        sTranslationConfig.addTargetLanguage("es");
        sTranslationConfig.addTargetLanguage("hi");        
        log('Ready to translate from ' + sTranslationConfig.speechRecognitionLanguage);

        recursiveAsyncReadLine();

    }
    catch  (error) {
        // Something went wrong, write the error
        log(error)
    }
}


function  Translate(targetLanguage) {

    return new Promise(resolve => {
        // Translate speech
        const mic = new Mic({rate: 8000, bitwidth : 8});
        const micStream = mic.startRecording();

        micStream.on('data', d => pushStream.write(d.slice()));
        micStream.on('end', () => {
            pushStream.close();
        });
                        
        setTimeout(function () {
            mic.stopRecording(); // Stop recording after 5sec
        }, 5000)
                        
        var pushStream = sdk.AudioInputStream.createPushStream();
        var audioConfig = sdk.AudioConfig.fromStreamInput(pushStream);
        var translator = new sdk.TranslationRecognizer(sTranslationConfig, audioConfig);
        log('Speak now...');

        translator.recognizeOnceAsync(
                function (result) {
                    log(`Translating: "${result.text}"`);
                    var translation = result.translations.get(targetLanguage);
                    //log('translation:' + translation)
                    resolve(translation);
                    translator.close();
                },
                function (err) {
                    log(err);
                    translator.close();
                });
    });
    
}

function SpeechSynthesize(targetLanguage, toSynthesize) {
    return new Promise(resolve => {
        
        // Configure speech
        speechConfig = sdk.SpeechConfig.fromSubscription(cog_key, cog_region)

        // Configure speech synthesis
        var voices = {
            "fr": "fr-FR-Julie",
            "es": "es-ES-Laura",
            "hi": "hi-IN-Kalpana"
        };

        var pstream = sdk.AudioOutputStream.createPullStream();
        var synaudioConfig = sdk.AudioConfig.fromStreamOutput(pstream);
        var speaker = new Speaker({ channels: 1, sampleRate: 16000, bitDepth: 16 });

        speechConfig.speechSynthesisVoiceName = voices[targetLanguage]; 
        var synthesizer = new sdk.SpeechSynthesizer(speechConfig, synaudioConfig);

        synthesizer.speakTextAsync(
                toSynthesize,
                result => {
                    if (result) {
                        synthesizer.close();
                        let bufferStream = new stream.PassThrough();
                        bufferStream.end(Buffer.from(result.audioData));
                        bufferStream.pipe(speaker);
                        resolve(result.reason);
                    }
                },
                error => {
                    console.log(error);
                    synthesizer.close();
                });
        
    });
}


var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

var recursiveAsyncReadLine = async function () {
    rl.question('\nEnter a target language\n fr = French\n es = Spanish\n hi = Hindi\n Enter anything else to stop\n', async function (command) {
      if (command.toLowerCase() == 'quit') 
       return rl.close(); 
    
      var translated = await Translate(command);
      log(translated);
      
      await SpeechSynthesize(command,translated);

      recursiveAsyncReadLine();       
  });
};

main();