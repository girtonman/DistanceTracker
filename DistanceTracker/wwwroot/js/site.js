// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

Chart.defaults.color = '#eee';


function timeFromMs(milliseconds, logging)
{
	var seconds = milliseconds / 1000.0;
	var minutes = seconds / 60.0;
	var hours = minutes / 60.0;
	if (logging)
		console.log(milliseconds, hours, minutes, seconds);
	seconds = Math.trunc(seconds % 60);
	minutes = Math.trunc(minutes % 60);
	hours = Math.trunc(hours);
	if(logging)
		console.log(milliseconds, hours, minutes, seconds);

	var output = '';
	output += hours >= 1 ? `${hours}:` : '';
	output += hours >= 1 ? minutes >= 10 ? `${minutes}:` : `0${minutes}:` : minutes >= 1 ? `${minutes}:` : '';
	output += minutes >= 1 ? seconds >= 10 ? `${seconds}` : `0${seconds}` : seconds >= 1 ? `${seconds}` : '';
	output += milliseconds%1000 != 0 ? `.${Math.trunc(milliseconds % 1000.0)}s` : 's';

	return output;
}
