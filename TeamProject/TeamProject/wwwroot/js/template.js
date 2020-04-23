const logo = document.querySelector('.logo');
const navbarCustom = document.querySelector('.navbar-custom');
const customLink = document.querySelector('.custom-link');
const linkText = document.querySelector('.link-text');
const main = document.querySelector('main');
let active;
logo.addEventListener('click', () => {
  navbarCustom.classList.toggle('navbar-custom-active');
  main.classList.toggle('main-active');
});
console.log(main);

