const http = require("http");

const port = Number(process.env.PORT) || 3000;

const server = http.createServer((req, res) => {
  if (req.url === "/health") {
    res.writeHead(200, { "Content-Type": "text/plain" });
    res.end("Healthy");
    return;
  }

  // Returns the value of an environment variable, to check the configuration given to the application.
  const match = req.url.match(/^\/env\/(.+)$/);
  if (match) {
    const value = process.env[decodeURIComponent(match[1])];
    res.writeHead(value === undefined ? 404 : 200, { "Content-Type": "text/plain" });
    res.end(value ?? "");
    return;
  }

  res.writeHead(404);
  res.end();
});

server.listen(port, () => console.log(`Listening on http://localhost:${port}`));
