//var calendar = document.getElementById("calendar");

//function getMonthName(month) {
//  var monthNames = [
//    "January",
//    "February",
//    "March",
//    "April",
//    "May",
//    "June",
//    "July",
//    "August",
//    "September",
//    "October",
//    "November",
//    "December",
//  ];
//  return monthNames[month];
//}

//function getDayName(day) {
//  var dayNames = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
//  return dayNames[day];
//}

//function Month(month, year) {
//  var monthYear = document.getElementById("month-year");
//  monthYear.textContent = getMonthName(month) + " " + year;
//}

//document
//  .getElementById("prev-month-btn")
//  .addEventListener("click", function () {
//    currentMonth--;
//    if (currentMonth < 0) {
//      currentMonth = 11;
//      currentYear--;
//    }
//    Month(currentMonth, currentYear);
//    generateCalendar(currentMonth, currentYear);
//  });

//document
//  .getElementById("next-month-btn")
//  .addEventListener("click", function () {
//    currentMonth++;
//    if (currentMonth > 11) {
//      currentMonth = 0;
//      currentYear++;
//    }
//    Month(currentMonth, currentYear);
//    generateCalendar(currentMonth, currentYear);
//  });

//var currentDate = new Date();
//var currentYear = currentDate.getFullYear();
//var currentMonth = currentDate.getMonth();
//var currentDay = currentDate.getDate();

//Month(currentMonth, currentYear);

//var weeksArray;
//function generateCalendar(monthIndex, year) {
//  let days = [];
//  weeksArray = [];
//  let firstday = createDay(1, monthIndex, year);
//  let prevMonth = monthIndex !== 0 ? monthIndex - 1 : 11;
//  let prevMonthYear = monthIndex !== 0 ? year : year - 1;
//  let countPrevMonthDays = new Date(prevMonthYear, prevMonth, 0).getDate();

//  // Prev month
//  for (
//    let i = 0, j = countPrevMonthDays;
//    i < firstday.weekDayNumber;
//    j--, i++
//  ) {
//    days.push(createDay(j, prevMonth, prevMonthYear));
//  }
//  days.reverse();
//  days.push(firstday);

//  let countDaysInMonth = new Date(year, monthIndex + 1, 0).getDate();
//  for (let i = 2; i < countDaysInMonth + 1; i++) {
//    days.push(createDay(i, monthIndex, year));
//  }

//  // Next month
//  const numEmptyDays = 7 - (days.length % 7);
//  if (numEmptyDays < 7) {
//    for (let i = 1; i < numEmptyDays + 1; i++) {
//      days.push(
//        createDay(
//          i,
//          monthIndex !== 11 ? monthIndex + 1 : 0,
//          monthIndex !== 11 ? year : year + 1
//        )
//      );
//    }
//  }
//  weeksArray = days.reduce((acc, day, index) => {
//    const weekIndex = Math.floor(index / 7);
//    if (!acc[weekIndex]) {
//      acc[weekIndex] = [];
//    }
//    acc[weekIndex].push(day);
//    return acc;
//  }, []);
//  resetCalendar();
//  WeekDay();
//  displayDaysNumber();
//}

//function createDay(dayNumber, monthIndex, year) {
//  let weekDayNumber = new Date(year, monthIndex, dayNumber).getDay();
//  let day = {
//    monthIndex: monthIndex,
//    month: getMonthName(monthIndex),
//    day: dayNumber,
//    year: year,
//    weekDayNumber: weekDayNumber,
//    weekDayName: getDayName(weekDayNumber),
//  };
//  return day;
//}
////function selectDay(selected) {
////    var date = new Date(selected.year, selected.monthIndex, selected.day);
  
////    var selectedSpan = document.querySelector(".selected-day");
////    if (selectedSpan) {
////      selectedSpan.classList.remove("selected-day");
////    }
  
////    var span = event.target;
////    if (span.nodeName === "SPAN") {
////      span.classList.add("selected-day");
////    }

////    var formattedDate = date.toLocaleDateString('en-US', {
////      weekday: 'long',
////      month: 'long',
////      day: 'numeric'
////    });
    
////    document.getElementById('current-day').innerHTML = formattedDate;

////  }

//function displayDaysNumber() {
//  for (i = 0; i < weeksArray.length; i++) {
//    for (j = 0; j < 7; j++) {
//      var td = document.createElement("td");
//      td.innerHTML = `<span class='calenderDay py-1 px-2 rounded'>${weeksArray[i][j].day}</span>`;
//      (function (i, j) {
//        td.onclick = function () {
//          selectDay(weeksArray[i][j]);
//        };
//      })(i, j);
//      calendar.children[j].appendChild(td);
//    }
//  }
//}

//function WeekDay() {
//  for (var col = 0; col < 7; col++) {
//    var weekRow = document.createElement("tr");
//    weekRow.innerHTML = "<th scope='row'>" + getDayName(col) + "</th>";
//    calendar.appendChild(weekRow);
//  }
//}
//function resetCalendar() {
//  calendar.innerHTML = "";
//}

//generateCalendar(currentMonth, currentYear);
