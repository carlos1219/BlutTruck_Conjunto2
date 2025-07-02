<template>
    <div id="hero" class="flex flex-col pt-6 px-6 lg:px-20 overflow-hidden"></div>
    <div style="background: linear-gradient(to bottom, rgba(255, 255, 255, 0.5), rgba(255, 255, 255, 1)), radial-gradient(100% 150% at 50% 0%, rgb(89, 150, 255) 0%, rgb(255, 255, 255) 100%); clip-path: ellipse(150% 87% at 93% 13%)">
        <div class="mx-6 md:mx-20 mt-0 md:mt-6 text-center">
            <h1 class="text-6xl font-bold text-gray-900 leading-tight">
                <span class="font-light block">La aplicación que se preocupa por ti y por los tuyos</span>
                Pruebela gratis ahora
            </h1>
            <div class="flex justify-center mt-8">
                <Button label="Empieza Ahora" as="router-link" to="/auth/login" rounded class="!text-xl !px-6 !py-3 !bg-blue-500 text-white mb-6 !border-blue-500"></Button>
            </div>
        </div>
        <div class="flex justify-center mt-8">
            <img src="/demo/images/landing/hero_image.png" alt="Hero Image" class="w-9/12 md:w-auto" />
        </div>
    </div>
</template>
