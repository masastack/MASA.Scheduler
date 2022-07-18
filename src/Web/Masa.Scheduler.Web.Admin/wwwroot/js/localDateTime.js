async function toLocalTime(datetimeOffset) {
    const date = new Date(datetimeOffset);
    const dateStr = date.toLocaleString();
    return dateStr;
}