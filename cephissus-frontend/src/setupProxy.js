const { createProxyMiddleware } = require("http-proxy-middleware");

module.exports = function (app) {
    app.use(
        ["/auth", "/config"],
        createProxyMiddleware({
            target: "http://localhost:6969",
            changeOrigin: true,
            secure: false,
        })
    )
}