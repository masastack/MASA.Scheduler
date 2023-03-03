window._blazorDownloadFile=function(url, filename) {
    const xhr = new XMLHttpRequest();
    xhr.open('GET', url, true);
    xhr.responseType = 'blob';
    xhr.onload = () => {
        if (xhr.status === 200) {
            var blob = xhr.response;
            if (window.navigator.msSaveOrOpenBlob) {
                navigator.msSaveBlob(blob, filename);
            } else {
                const link = document.createElement('a');
                const body = document.querySelector('body');

                link.href = window.URL.createObjectURL(blob);
                link.download = filename;
                link.style.display = 'none';
                body.appendChild(link);

                link.click();
                body.removeChild(link);

                window.URL.revokeObjectURL(link.href);
            }
        }
    };
    xhr.send();
}
