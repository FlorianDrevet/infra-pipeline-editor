# Template Angular Project

Init with :

- Angular 19 (standalone components, zoneless change detection)
- Tailwind CSS 3
- Angular Material 19

# Tailwind

Possible to add font, add in tailwind.config.js :
```
theme: {
    fontFamily: {
      'wedding': ["Wedding"],
      "windsong": ["WindSong"],
      "librebaskerville": ["LibreBaskerville"],
    },
    extend: {},
  },
```

# Material Angular

## Theme
A custom theme is created in styles.scss: https://material.angular.io/guide/theming#defining-a-theme


# TODO when starting app

It is possible to navigate between the different TODO thanks to the IDE. This is the list of the different changes to do at the init:

# Name Application

It is important to go in package.json and change the name of the application. 

## Title 

Change in index.html the title of the website
Change in app.component.ts the title of the website

# Icon Website

Add in public/ folder the icon of the app. Change in index.html the path to the icon

# Azurite command

azurite --silent --location c:\azurite --debug c:\azurite\debug.log
