var video = document.getElementById("myVideo");
var playButton = document.querySelector(".play");

playButton.addEventListener("click", function () {
    if (video.paused) {
        video.play();
    } else {
        video.pause();
    }
});