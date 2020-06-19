// Template for webpack.config.js in Fable projects
// Find latest version in https://github.com/fable-compiler/webpack-config-template

// In most cases, you'll only need to edit the CONFIG object (after dependencies)
// See below if you need better fine-tuning of Webpack options

// Dependencies. Also required: core-js, fable-loader, fable-compiler, @babel/core,
// @babel/preset-env, babel-loader, sass, sass-loader, css-loader, style-loader, file-loader
var path = require("path");
var webpack = require("webpack");
var CopyWebpackPlugin = require('copy-webpack-plugin');
var MiniCssExtractPlugin = require("mini-css-extract-plugin");

var CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    fsharpEntry: "./src/Client/Client.fsproj",
    cssEntry: "./src/Client/style.sass",
    outputDir: "./src/Client/public",
    assetsDir: "./src/Client/public",
    devServerPort: 8085,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {
        '**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8080"),
               changeOrigin: true
           }
    },
    // Use babel-preset-env to generate JS compatible with most-used browsers.
    // More info at https://babeljs.io/docs/en/next/babel-preset-env.html
    babel: {
        presets: [
            ['@babel/preset-env', {
                modules: false,
                // This adds polyfills when needed. Requires core-js dependency.
                // See https://babeljs.io/docs/en/babel-preset-env#usebuiltins
                useBuiltIns: 'usage',
                corejs: 3
            }]
        ],
    }
}

// If we're running the webpack-dev-server, assume we're in development mode
var isProduction = false //!process.argv.find(v => v.indexOf('webpack-dev-server') !== -1);
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var commonPlugins = [];

module.exports = env => {
    console.log("Version: " + env.VERSION)
    var version = env.VERSION;

    return {
        // In development, bundle styles together with the code so they can also
        // trigger hot reloads. In production, put them in a separate CSS file.
        entry: isProduction ? {
            app: [resolve(CONFIG.fsharpEntry), resolve(CONFIG.cssEntry)]
        } : {
                app: [resolve(CONFIG.fsharpEntry)],
                style: [resolve(CONFIG.cssEntry)]
            },
        // Add a hash to the output file name in production
        // to prevent browser caching if code changes
        output: {
            path: resolve(CONFIG.outputDir),
            filename: '[name].'+ version +'.js'
        },
        mode: isProduction ? "production" : "development",
        devtool: isProduction ? "source-map" : "eval-source-map",
        optimization: {
            // Split the code coming from npm packages into a different file.
            // 3rd party dependencies change less often, let the browser cache them.
            splitChunks: {
                cacheGroups: {
                    commons: {
                        test: /node_modules/,
                        name: "vendors",
                        chunks: "all"
                    }
                }
            },
        },
        // Besides the HtmlPlugin, we use the following plugins:
        // PRODUCTION
        //      - MiniCssExtractPlugin: Extracts CSS from bundle to a different file
        //          To minify CSS, see https://github.com/webpack-contrib/mini-css-extract-plugin#minimizing-for-production
        //      - CopyWebpackPlugin: Copies static assets to output directory
        // DEVELOPMENT
        //      - HotModuleReplacementPlugin: Enables hot reloading when code changes without refreshing
        plugins: isProduction ?
            commonPlugins.concat([
                new MiniCssExtractPlugin({ filename: 'style.css' }),
                new CopyWebpackPlugin([{ from: resolve(CONFIG.assetsDir) }]),
            ])
            : commonPlugins.concat([
                new webpack.HotModuleReplacementPlugin(),
            ]),
        resolve: {
            // See https://github.com/fable-compiler/Fable/issues/1490
            symlinks: false
        },
        // Configuration for webpack-dev-server
        devServer: {
            publicPath: "/",
            contentBase: resolve(CONFIG.assetsDir),
            port: CONFIG.devServerPort,
            proxy: CONFIG.devServerProxy,
            hot: true,
            inline: true
        },
        // - fable-loader: transforms F# into JS
        // - babel-loader: transforms JS to old syntax (compatible with old browsers)
        // - sass-loaders: transforms SASS/SCSS into JS
        // - file-loader: Moves files referenced in the code (fonts, images) into output folder
        module: {
            rules: [
                {
                    test: /\.fs(x|proj)?$/,
                    use: {
                        loader: "fable-loader",
                        options: {
                            babel: CONFIG.babel
                        }
                    }
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: CONFIG.babel
                    },
                },
                {
                    test: /\.(sass|scss|css)$/,
                    use: [
                        isProduction
                            ? MiniCssExtractPlugin.loader
                            : 'style-loader',
                        'css-loader',
                        {
                          loader: 'sass-loader',
                          options: { implementation: require("sass") }
                        }
                    ],
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
                    use: ["file-loader"]
                }
            ]
        }
    };
};

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}
