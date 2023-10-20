var video = document.getElementById("myVideo");
var playButton = document.getElementById("playButton");

playButton.addEventListener("click", function () {
    if (video.paused) {
        video.play();
        playButton.style.display = "none";
    } else {
        video.pause();
        playButton.style.display = "block";
    }
});

video.addEventListener("click", function () {
    if (video.paused) {
        video.play();
        playButton.style.display = "none";
    } else {
        video.pause();
        playButton.style.display = "block";
    }
});