// Encapsulate the Media Interaction
console.log("Start loading PlayVideo.js");

let mediaSource = null;
let sourceBuffer = null;

export async function UpdateSourceURL   () {
    const medSrc = URL.createObjectURL(mediaSource);
    console.log(`ObjectURL=${medSrc}`);
   
    return medSrc;
}

export function CreateMediaSource(mediaType) {

    mediaSource = new MediaSource();
    mediaSource.mediaType = mediaType;
    console.log(`createdMediaSource(${mediaType}) - MediaSource() constructed`);

    mediaSource.addEventListener('sourceopen', function () {
        console.log("sourceopen event fire");
        sourceBuffer = mediaSource.addSourceBuffer('video/webm; codecs="vorbis,vp8"');
        sourceBuffer.mode = 'sequence';
    });
}


export function AttachSegment(segmentData) {
    if (sourceBuffer === null) return -1;  // Source not attached
    const sourceBufferList = mediaSource.activeSourceBuffers;

    if (segmentData === null) return sourceBufferList.length;

    mediaSource.addSegment(segmentData);

    return sourceBufferList.length;
};



console.log("Loaded PlayVideo.js");






//var mediaSource = new MediaSource();
//video.src = window.URL.createObjectURL(mediaSource);
//mediaSource.addEventListener('sourceopen', function () {
//    var sourceBuffer =
//        mediaSource.addSourceBuffer('video/webm; codecs="vorbis,vp8"');
//    // Get video segments and append them to sourceBuffer.
//}

//    reader.onload = function (e) {
//        sourceBuffer.appendBuffer(new Uint8Array(e.target.result));
//        if (i === NUM_CHUNKS - 1) {
//            mediaSource.endOfStream();
//        } else {
//            if (video.paused) {
//                // start playing after first chunk is appended
//                video.play();
//            }
//            readChunk(++i);
//        }
//    };