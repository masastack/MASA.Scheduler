function autoHeight() {   
    let table = document.getElementsByClassName("error-detail")[0];
    if (!table) return;
    table = table.getElementsByTagName("table")[0];
    if (!table) return;
    let parent = table?.parentNode;
    if (!table || !parent) return;
    let t = setTimeout(function () {
        parent.style.height = (table.offsetHeight + 10) + "px";
        clearTimeout(t);
    }, 100)

}

export { autoHeight }