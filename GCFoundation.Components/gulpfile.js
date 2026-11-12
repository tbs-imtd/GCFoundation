import gulp from 'gulp';
import dartSass from 'sass';
import gulpSass from 'gulp-sass';
import cleanCSS from 'gulp-clean-css';
import sourcemaps from 'gulp-sourcemaps';
import uglify from 'gulp-uglify';
import concat from 'gulp-concat';
import babel from 'gulp-babel';
import rename from 'gulp-rename';
import autoprefixer from 'gulp-autoprefixer';
import newer from 'gulp-newer';
import { deleteAsync } from 'del';
import plumber from 'gulp-plumber';
import notify from 'gulp-notify';
import through2 from 'through2';

const { src, dest, watch, series, parallel } = gulp;
const sass = gulpSass(dartSass);

// Development mode flag
const isDev = process.env.NODE_ENV === 'development';

// Paths
const paths = {
    scss: {
        // Entry files only; partials (prefixed with "_") are excluded
        entries: [
            'wwwroot/src/scss/**/*.scss',
            '!wwwroot/src/scss/**/_*.scss'
        ],
        watch: 'wwwroot/src/scss/**/*.scss',
        dest: 'wwwroot/css'
    },
    js: {
        src: 'wwwroot/src/js/**/*.js',
        vendor: [
            'node_modules/tabbable/dist/index.umd.js',
            'node_modules/focus-trap/dist/focus-trap.umd.js'
        ],
        dest: 'wwwroot/js'
    },
    vendor: {
        quill: {
            js: 'node_modules/quill/dist/quill.js',
            css: 'node_modules/quill/dist/quill.snow.css',
            dest: 'wwwroot/lib/quill',
            cssDest: 'wwwroot/lib/quill'
        }
    }
};

// Error handling
const errorHandler = {
    errorHandler: notify.onError({
        title: 'Error',
        message: '<%= error.message %>'
    })
};

// Clean generated files
async function clean() {
    await deleteAsync([
        paths.scss.dest + '/*',
        paths.js.dest + '/*'
    ]);
}

// Compile SCSS
function styles() {
    return src(paths.scss.entries)
        .pipe(plumber(errorHandler))
        .pipe(sourcemaps.init())
        .pipe(sass({
            includePaths: ['wwwroot/src/scss'],
            outputStyle: 'compressed'
        }).on('error', sass.logError))
        .pipe(autoprefixer({ cascade: false }))
        .pipe(cleanCSS())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(dest(paths.scss.dest));
}

// Bundle and minify JS
function scripts() {
    return src([...paths.js.vendor, paths.js.src])
        .pipe(plumber(errorHandler))
        .pipe(sourcemaps.init())
        .pipe(babel({
            presets: ['@babel/preset-env']
        }))
        .pipe(concat('foundation.js'))
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(dest(paths.js.dest));
}

// Copy Quill vendor assets
function copyQuillJs() {
    return src(paths.vendor.quill.js)
        .pipe(plumber(errorHandler))
        .pipe(newer(paths.vendor.quill.dest))
        .pipe(dest(paths.vendor.quill.dest));
}

function copyQuillCss() {
    return src(paths.vendor.quill.css)
        .pipe(plumber(errorHandler))
        .pipe(newer(paths.vendor.quill.cssDest))
        .pipe(dest(paths.vendor.quill.cssDest));
}

// Build tasks
export const build = series(
    clean,
    parallel(styles, scripts, copyQuillJs, copyQuillCss)
);

// Watch task (run separately with 'gulp watch')
export const watchTask = function() {
    // Initial build
    build();
    
    // Then watch for changes
    watch(paths.scss.watch, styles);
    watch(paths.js.src, scripts);
};

// Default task
export default build;

function noop() {
    return through2.obj();
}