import Vue from 'vue';
import VueCountdown from '@chenfengyuan/vue-countdown';

const Countdown = {
  install(Vue, options) {
    Vue.component('vue-countdown', VueCountdown)
  }
};

Vue.use(Countdown);
export default Countdown;

