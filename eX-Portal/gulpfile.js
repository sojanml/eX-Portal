/// <binding ProjectOpened='default' />
var gulp = require('gulp');
var compass = require('gulp-compass');
var plumber = require('gulp-plumber');

gulp.task('sass', function () {
  return gulp.src(__dirname + 'ex-Portal/sass/**/*')
    .pipe(plumber(function (error) {
      console.log(error.message);
      this.emit('end');
    }))
    .pipe(compass({
      config_file: __dirname + '\\ex-Portal\\config.rb',
      project: __dirname + '/ex-Portal',
      css: 'Content',
      sass: 'sass'
    }))
    .pipe(gulp.dest('ex-Portal'));
});

//C:\Web\eX-Portal\eX-Portal\eX-Portal\

//Watch task
gulp.task('default', function () {
  var watcher = gulp.watch('ex-Portal/sass/**/*', ['sass']);

  watcher.on('change', function (event) {
    console.log('File ' + event.path + ' was ' + event.type);
  });

  watcher.on('error', function (event) {
    console.log('File ' + event.path + ' was ' + event.type);
  });

});

