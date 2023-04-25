<template>
  <nav class="bg-[#19212b] top-0 inset-x-0 w-full z-30 text-white fixed transition duration-300 delay-0 ease-out" :class="{'transition-right': showPopup}">
            <div class="relative container sm:px-18 mx-auto px-4">
                <div class="w-full flex justify-between flex-row-reverse">
                    <div class="flex md:space-x-7 md:w-full">
                        <!-- Website Logo -->
                        <div class="w-full">
                            <nuxt-link to="/" class="flex items-center py-4 px-2 w-[150px]">
                                <img
                                    class="w-[150px] h-14 object-contain"
                                    src="/images/svg/epson-logo.png"
                                    alt="Epson" />
                            </nuxt-link>
                        </div>
                        <!-- Primary Navbar items -->
                        <div class="hidden md:flex items-center w-full justify-end ">
                            <nuxt-link
                                to="/"
                                class="w-40  h-full hover:bg-[#003399] flex justify-center items-center  font-semibold transition duration-300"
                                >Home</nuxt-link
                            >
                            <a
                                href="https://virtualshowroom.epson.com.my/"
                                class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300"
                                target="_blank"
                                >Virtual Showroom</a
                            >
                            <a
                                href="https://s.lazada.com.my/s.V0n6K"
                                class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300"
                                target="_blank"
                                >Lazada Store</a
                            >
                            <a
                                href="https://shopee.com.my/epson.os"
                                class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300"
                                target="_blank"
                                >Shopee Store</a
                            >
                        </div>
                    </div>
                    <!-- Mobile menu button -->
                    <div class="md:hidden flex items-center">
                        <button class="outline-none" @click="openPopup()">
                            <svg
                                class="w-6 h-6 text-white"
                                x-show="!showMenu"
                                fill="none"
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                stroke-width="2"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                            >
                            <path d="M4 6h16M4 12h16M4 18h16"></path>
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </nav>
</template>

<script>
import Vue from 'vue'
import EventBus from '~/components/eventbus'
Vue.directive('scroll', {
    inserted (el, binding) {
        const f = function (event) {
            if (binding.value(event, el)) {
                window.removeEventListener('scroll', f)
            }
        }
        window.addEventListener('scroll', f)
    }
})
export default {
    name: 'Header',
    data() {

    },
    components: {
        EventBus
    },
    methods: {
        updateScroll () {
            this.scrollPosition = window.scrollY
        },
        openPopup () {
            this.showPopup = true;
            window.scrollTo(0, 0);
            document.body.classList.add('stop-scrolling')
            EventBus.$emit('OPEN_MOBILE_HEADER', this.showPopup)
        },
    },
    mounted() {
        console.log('epson-header-mounted');
        const $vm = this
        EventBus.$on('OPEN_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
        EventBus.$on('CLOSE_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
    }
}
</script>

<style>
.homeHeader {
    background-color: #f1f3f5;
    color: rgb(107 114 128);
}
.homePageMobile {
    background-color: #f1f3f5;
}
.homePageIsActive {
    color: #003399;
    border-color: #003399;
    border-bottom-width: 4px;
}

.homePageIsActiveScrolled {
    color: #003399;
    border-color: #003399;
}

.homePageIsActiveMobile {
    background-color: #003399;
    color:white
}

.stop-scrolling {
  height: 100%;
  overflow: hidden
}

.transition-right{
    transform: translate(100%, 0);
}
</style>