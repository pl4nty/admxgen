import React, { useState } from 'react';
import axios from 'axios';

function App() {
    const [jsonFile, setJsonFile] = useState(null);
    const [csvFile, setCsvFile] = useState(null);
    const [admxFile, setAdmxFile] = useState(null);
    const [admlFile, setAdmlFile] = useState(null);

    const handleJsonFileChange = (event) => {
        setJsonFile(event.target.files[0]);
    };

    const handleCsvFileChange = (event) => {
        setCsvFile(event.target.files[0]);
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        const formData = new FormData();
        formData.append('jsonFile', jsonFile);
        formData.append('csvFile', csvFile);

        try {
            const response = await axios.post('/api/generate', formData, {
                responseType: 'blob',
            });

            const admxBlob = new Blob([response.data.admx], { type: 'application/xml' });
            const admlBlob = new Blob([response.data.adml], { type: 'application/xml' });

            setAdmxFile(URL.createObjectURL(admxBlob));
            setAdmlFile(URL.createObjectURL(admlBlob));
        } catch (error) {
            console.error('Error generating files:', error);
        }
    };

    return (
        <div className="App">
            <h1>ADMXGen Web Interface</h1>
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="jsonFile">JSON File:</label>
                    <input type="file" id="jsonFile" accept=".json" onChange={handleJsonFileChange} />
                </div>
                <div>
                    <label htmlFor="csvFile">CSV File:</label>
                    <input type="file" id="csvFile" accept=".csv" onChange={handleCsvFileChange} />
                </div>
                <button type="submit">Generate</button>
            </form>
            {admxFile && (
                <div>
                    <h2>Generated Files</h2>
                    <a href={admxFile} download="output.admx">Download ADMX File</a>
                    <a href={admlFile} download="output.adml">Download ADML File</a>
                </div>
            )}
        </div>
    );
}

export default App;
