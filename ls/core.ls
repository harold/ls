;; Binding test
(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error))))

;; Helpers
(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))
(define read (fn (str) (. 'ls.Reader 'Read str)))
(define eval (fn (str) (. 'ls.Evaluator 'Eval str *env*)))

;; Math
(define pi (. 'System.Math 'PI))
(define tau (* pi 2))
(define sin (fn (x) (. 'System.Math 'Sin x)))
(define cos (fn (x) (. 'System.Math 'Cos x)))
(define tan (fn (x) (. 'System.Math 'Tan x)))
(define rad (fn (x) (* pi (/ x 180.0))))
(define deg (fn (x) (* x (/ 180.0 pi))))

;; Looping
;; example: (dotimes 3 '(print "yo"))
(define dotimes (fn (i expr)
  (if (= i 0)
    'done
    (begin
      (eval expr)
      (dotimes (- i 1) expr)))))

;; Sqrt (with apologies to Abelson, Sussman, et. al.)
(define sqrt-iter (fn (guess x)
  (if (good-enough? guess x)
      guess
      (sqrt-iter (improve guess x) x))))

(define improve (fn (guess x)
  (average guess (/ x guess))))

(define average (fn (x y)
  (/ (+ x y) 2.0)))

(define square (fn (x) (* x x)))

(define abs (fn (x) (if (< x 0) (- 0 x) x)))

(define good-enough? (fn (guess x)
  (< (abs (- (square guess) x)) 0.001)))

(define sqrt (fn (x) (sqrt-iter 1.0 x)))

;; Logic
(define not (fn (b) (if b false true)))

;; Lists
(define cons  (fn (a b) (if (not b) `(,a) (begin (. b 'Insert 0 a) b))))
(define first (fn (l) (. l 'get_Item 0)))
(define rest  (fn (l) (. l 'GetRange 1 (- (. l 'Count) 1))))
(define map   (fn (f l) (if (= (. l 'Count) 0) null (cons (f (first l)) (map f (rest l))))))

;; Hashtables
(define assoc (fn (h k v) (begin (. h 'set_Item k v) h)))
